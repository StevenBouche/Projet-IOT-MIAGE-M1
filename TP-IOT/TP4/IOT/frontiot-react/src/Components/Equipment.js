import React, { Component } from 'react';
import {Row, Container, Form, Col} from 'react-bootstrap'
import {Line} from 'react-chartjs-2';
import DataService from '../Services/DataService'

class Equipment extends Component {

  constructor(props) {
    super(props);
 
    this.state = {
      datas : [],
      stateChart : {},
      nbData : 20, 
      configChart: {
        responsive: true,
        maintainAspectRatio: true,
        title:{
          display:true,
          text:'Data of equipment',
          fontSize:20
        },
        legend:{
          display:true,
          position:'right'
        }
      }
    }
  }

  async getData(id){
    return await DataService.getLastData(id,this.state.nbData);
  }

  timestampToDateString(timestamp){
    var date = new Date(timestamp*1000);
    return date.getDate()+
    "/"+(date.getMonth()+1)+
    "/"+date.getFullYear()+
    " "+date.getHours()+
    ":"+date.getMinutes()+
    ":"+date.getSeconds();
  }

  async componentDidMount() {
      await this.pullData(this.props.obj.equipmentId);
      this.interval = setInterval(async () => {
        await this.updateChart(this)
      }, 1000);
  }

  async pullData(id){
      var datas = await this.getData(id);
      if(datas!==undefined&&datas.length>0){
        var stateChart = this.updateStateChart(datas);
        this.setState({stateChart: stateChart, datas: datas});
      }
  }

    updateStateChart(datas){

      var labels = [];
      var dataTemp = [];
      var dataLum = [];

      datas.forEach(data => {
          labels.push(this.timestampToDateString(data.timestamp))
          dataTemp.push(data.temperature);
          dataLum.push(data.ligth);
      });

      return { labels: labels, datasets: [ this.getDatasets("Temperature",dataTemp,'rgba(0,255,0,0.3)'), this.getDatasets("Ligth",dataLum,'rgba(75,192,192,1)') ] }

    }

    async componentWillUnmount() {
      clearInterval(this.interval);
    }

    async updateChart(component){

        var id = component.props.obj.equipmentId;
        var lastTimestamp = component.state.datas[component.state.datas.length-1].timestamp;
        
        var data = await DataService.getLastDataUpdate(id,lastTimestamp);

        if(data!==undefined&&data.length>0){

          var datas = component.state.datas;

          data.forEach(d => {
              datas.push(d);
          })

          var stateChart = component.updateStateChart(datas);

          component.setState({stateChart: stateChart, datas: datas});

        }
      
    }

    getDatasets(label,data,bordercolor){
        return {
          label: label,
          fill: false,
          lineTension: 0.5,
          backgroundColor: 'rgba(75,192,192,1)',
          borderColor: bordercolor,
          borderWidth: 2,
          data: data
        };
    }

    async handleChange(e){
      e.preventDefault();
      var value = e.target.value;
      this.setState({ nbData: value }, async () => {
        await this.pullData(this.props.obj.equipmentId);
      });
    }

    render() {

      let id = "id"+this.props.obj.equipmentId;

      return <div className={id}>
        <Container style={{ padding: '1%' }} fluid>
          <Row><h2>Equipment {this.props.obj.equipmentId}</h2></Row>
          <Row style={{ marginTop: '1%' }}>
            <Col sm={12}>
              <Form>
                <Form.Group controlId="formBasicRange">
                  <Form.Label><strong>Range value : </strong>{this.state.nbData}</Form.Label>
                  <Form.Control type="range" min="1" max="100" value={this.state.nbData} onChange={this.handleChange.bind(this)}/>
                </Form.Group>
              </Form>
            </Col>
          </Row>
          <Row>
            <Col sm={12}>
                <Line data={this.state.stateChart} options={this.state.configChart}/>
            </Col>
          </Row>
          
        </Container>
        
      </div>;
    }
}

export default Equipment;